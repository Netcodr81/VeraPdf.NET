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
package org.verapdf.gf.model.impl.external;

import org.verapdf.external.ICCProfile;
import org.verapdf.model.external.ICCInputProfile;

/**
 * Marks an ICC profile used as an input profile in a PDF color space.
 *
 * <p>ICC profiles describe color spaces through a binary header and tag data. This subclass assigns the input
 * profile model type while sharing header parsing, validity checks, and component-count diagnostics with
 * {@link GFICCProfile}.</p>
 *
 * <p><b>.NET porting notes:</b> Implement {@code VeraPdf.Model.External.IICCInputProfile} over a shared ICC
 * profile reader. The .NET framework has no direct equivalent to veraPDF's ICC parser; port
 * {@code org.verapdf.external.ICCProfile} and its validation checks rather than relying on color-management APIs
 * that may be platform-specific.</p>
 *
 * @author Maksim Bezrukov
 */
public class GFICCInputProfile extends GFICCProfile implements ICCInputProfile {

	/** Type name for {@code GFICCInputProfile} */
	public static final String ICC_INPUT_PROFILE_TYPE = "ICCInputProfile";

	/**
	 * Default constructor.
	 *
	 * @param iccProfile iccprofile object of profile
	 */
	public GFICCInputProfile(ICCProfile iccProfile) {
		super(iccProfile, ICC_INPUT_PROFILE_TYPE);
	}
}
