/*
 * This file is part of veraPDF Feature Reporting, a module of the veraPDF project.
 * Copyright (c) 2015-2026, veraPDF Consortium <info@verapdf.org>
 * All rights reserved.
 *
 * veraPDF Feature Reporting is free software: you can redistribute it and/or modify
 * it under the terms of either:
 *
 * The GNU General public license GPLv3+.
 * You should have received a copy of the GNU General Public License
 * along with veraPDF Feature Reporting as the LICENSE.GPL file in the root of the source
 * tree.  If not, see http://www.gnu.org/licenses/ or
 * https://www.gnu.org/licenses/gpl-3.0.en.html.
 *
 * The Mozilla Public License MPLv2+.
 * You should have received a copy of the Mozilla Public License along with
 * veraPDF Feature Reporting as the LICENSE.MPL file in the root of the source tree.
 * If a copy of the MPL was not distributed with this file, you can obtain one at
 * http://mozilla.org/MPL/2.0/.
 */
package org.verapdf.features.gf.objects;

import org.verapdf.features.objects.MetadataFeaturesObjectAdapter;
import org.verapdf.pd.PDMetadata;

import java.io.InputStream;
import java.util.Collections;
import java.util.List;

/**
 * Feature object adapter for metadata
 *
 * <p>Exposes the metadata stream to the feature reporter when the PDF
 * metadata object exists and is not empty. A missing object produces a null
 * stream rather than an empty stream; this adapter reports no errors itself.</p>
 *
 * <p><b>.NET port:</b> Use {@code VeraPdf.Parser.Pd.PdfMetadata} where its
 * API supports the operation, and map {@code InputStream} to
 * {@code System.IO.Stream}. Preserve stream ownership and disposal behavior
 * at the reporter boundary, and map an empty Java list to
 * {@code Array.Empty<string>()}.</p>
 *
 * @author Maksim Bezrukov
 */
public class GFMetadataFeaturesObjectAdapter implements MetadataFeaturesObjectAdapter {

    private final PDMetadata metadata;

    /**
     * Constructs new Metadata Feature Object adapter
     *
     * @param metadata class represents metadata object
     */
    public GFMetadataFeaturesObjectAdapter(PDMetadata metadata) {
        this.metadata = metadata;
    }

    @Override
    public InputStream getData() {
        if (metadata != null && !metadata.empty()) {
            return metadata.getStream();
        }
        return null;
    }

    @Override
    public boolean isPDFObjectPresent() {
        return this.metadata != null && !metadata.empty();
    }

    @Override
    public List<String> getErrors() {
        return Collections.emptyList();
    }
}
