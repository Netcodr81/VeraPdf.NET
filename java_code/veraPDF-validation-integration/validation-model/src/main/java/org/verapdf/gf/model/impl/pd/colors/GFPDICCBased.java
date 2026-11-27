/*
 * This file is part of veraPDF Validation, a module of the veraPDF project.
 * Copyright (c) 2015-2026, veraPDF Consortium <info@verapdf.org>
 * All rights reserved.
 *
 * veraPDF Validation is free software: you can redistribute it and/or modify
 * it under the terms of either:
 *
 * The GNU General public license GPLv3+.
 * You should have received a copy of the GNU General Public License
 * along with veraPDF Validation as the LICENSE.GPL file in the root of the source
 * tree.  If not, see http://www.gnu.org/licenses/ or
 * https://www.gnu.org/licenses/gpl-3.0.en.html.
 *
 * The Mozilla Public License MPLv2+.
 * You should have received a copy of the Mozilla Public License along with
 * veraPDF Validation as the LICENSE.MPL file in the root of the source tree.
 * If a copy of the MPL was not distributed with this file, you can obtain one at
 * http://mozilla.org/MPL/2.0/.
 */
package org.verapdf.gf.model.impl.pd.colors;

import org.verapdf.as.ASAtom;
import org.verapdf.external.ICCProfile;
import org.verapdf.gf.model.impl.containers.StaticContainers;
import org.verapdf.gf.model.impl.external.GFICCInputProfile;
import org.verapdf.model.baselayer.Object;
import org.verapdf.model.external.ICCInputProfile;
import org.verapdf.model.pdlayer.PDICCBased;
import org.verapdf.pd.colors.PDColorSpace;

import java.util.ArrayList;
import java.util.Collections;
import java.util.List;
import java.util.logging.Logger;

/**
 * Adapts an ICCBased color space and links its embedded ICC profile for conformance checks.
 *
 * <p>ICCBased color spaces use an embedded ICC profile to define component meaning.
 * This wrapper exposes the profile object, its MD5 digest, and its indirect object key.
 * It also checks that any alternate color space has the same number of components as
 * the ICC profile and reads the current transparency profile from per-document context.
 * These details are important for PDF/A color management.</p>
 *
 * <p><b>.NET porting notes:</b> Suggested type:
 * {@code VeraPdf.Validation.Gf.Model.Impl.Pd.Colors.GFPDICCBased}, implementing
 * {@code VeraPdf.Model.PDLayer.IPDICCBased}. Connect profile links to
 * {@code VeraPdf.Model.External.IICCInputProfile}; use {@code System.Security.Cryptography.MD5}
 * only if digest computation is needed, and replace static transparency context with
 * an injected document context. Use {@code ILogger<GFPDICCBased>} for warnings.</p>
 *
 * @author Maksim Bezrukov
 */
public class GFPDICCBased extends GFPDColorSpace implements PDICCBased {

    /** Logger for a mismatch between profile and alternate-space component counts. */
    private static final Logger LOGGER = Logger.getLogger(GFPDICCBased.class.getCanonicalName());

    /** Model type name for an ICCBased color space. */
    public static final String ICC_BASED_TYPE = "PDICCBased";

    /** Link name used to reach the external ICC profile model object. */
    public static final String ICC_PROFILE = "iccProfile";

    /** Creates an ICCBased adapter using the standard model type name. */
    public GFPDICCBased(org.verapdf.pd.colors.PDICCBased simplePDObject) {
        this(simplePDObject, ICC_BASED_TYPE);
    }

    /** Creates the ICCBased adapter and checks alternate-space component compatibility. */
    protected GFPDICCBased(org.verapdf.pd.colors.PDICCBased simplePDObject, String type) {
        super(simplePDObject, type);
        checkAlternateComponentsNumber();
    }

    @Override
    /** Resolves the ICC profile link, delegating any other links to the resource base. */
    public List<? extends Object> getLinkedObjects(String link) {
        if (ICC_PROFILE.equals(link)) {
            return this.getICCProfile();
        }
        return super.getLinkedObjects(link);
    }

	/** Wraps the embedded profile as an external-resource model object. */
	private List<ICCInputProfile> getICCProfile() {
        ICCProfile iccProfile = ((org.verapdf.pd.colors.PDICCBased) simplePDObject).getICCProfile();
        if (iccProfile != null) {
            List<ICCInputProfile> profiles = new ArrayList<>(MAX_NUMBER_OF_ELEMENTS);
            profiles.add(new GFICCInputProfile(iccProfile));
            return Collections.unmodifiableList(profiles);
        }
        return Collections.emptyList();
    }

    @Override
    /** Returns the profile's existing MD5 identifier. */
    public String getICCProfileMD5() {
        return ((org.verapdf.pd.colors.PDICCBased) simplePDObject).getICCProfileMD5();
    }

    @Override
    /** Returns the indirect key of the current transparency ICC profile, when applicable. */
    public String getcurrentTransparencyProfileIndirect() {
        PDColorSpace currentColorSpace = StaticContainers.getCurrentTransparencyColorSpace();
        if (currentColorSpace != null && ASAtom.ICCBASED.equals(currentColorSpace.getType())) {
            return ((org.verapdf.pd.colors.PDICCBased)currentColorSpace).getICCProfileIndirect();
        }
        return null;
    }

    @Override
    /** Returns the digest of the current transparency ICC profile, when applicable. */
    public String getcurrentTransparencyICCProfileMD5() {
        PDColorSpace currentColorSpace = StaticContainers.getCurrentTransparencyColorSpace();
        if (currentColorSpace != null && ASAtom.ICCBASED.equals(currentColorSpace.getType())) {
            return ((org.verapdf.pd.colors.PDICCBased)currentColorSpace).getICCProfileMD5();
        }
        return null;
    }

	/** Logs a parser diagnostic if the alternate space's component count is inconsistent. */
	private void checkAlternateComponentsNumber() {
        org.verapdf.pd.colors.PDICCBased colorSpace = ((org.verapdf.pd.colors.PDICCBased) simplePDObject);
        PDColorSpace alternate = colorSpace.getAlternate();
        if (alternate != null && colorSpace.getNumberOfComponents() != alternate.getNumberOfComponents()) {
            LOGGER.warning("Alternate color space does not match the number of components in the ICC profile");
        }

    }

    @Override
    /** Returns the indirect-object identifier of this ICC profile, if available. */
    public String getICCProfileIndirect() {
        return ((org.verapdf.pd.colors.PDICCBased) simplePDObject).getICCProfileIndirect();
    }
}
